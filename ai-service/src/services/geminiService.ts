import { config } from "../config";
import { logger } from "../utils/logger";
import { SentimentResult, TagExtractionResult, InsightsResult } from "../types";
import { processEmbeddingJob } from "../worker/embeddingWorker";
import { randomUUID } from "crypto";
import { isConfigured, model } from "../infra/geminiClient";

export const analyzeSentiment = async (
  text: string,
): Promise<SentimentResult> => {
  if (!isConfigured || !model) {
    return getFallbackSentiment(text);
  }

  try {
    const prompt = `Analyze the sentiment of the following text and respond with ONLY a JSON object in this exact format:
                    {"score": <number between -1 and 1>, "label": "<positive|negative|neutral>", "confidence": <number between 0 and 1>}

                    Text to analyze: "${text}"

                    Respond with only the JSON object, no other text.`;

    const result = await model.generateContent(prompt);
    const response = await result.response;
    const responseText = response.text().trim();

    const jsonMatch = responseText.match(/\{[\s\S]*\}/);
    if (jsonMatch) {
      const parsed = JSON.parse(jsonMatch[0]);
      return {
        score: Math.max(-1, Math.min(1, parsed.score)),
        label: parsed.label || "neutral",
        confidence: parsed.confidence || 0.8,
      };
    }

    throw new Error("Could not parse sentiment response");
  } catch (error) {
    const err = error as Error;
    logger.error("Sentiment analysis failed", { error: err.message });
    if (config.fallbackEnabled) {
      return getFallbackSentiment(text);
    }
    throw error;
  }
};

export const extractTags = async (
  text: string,
): Promise<TagExtractionResult> => {
  if (!isConfigured || !model) {
    return getFallbackTags(text);
  }

  try {
    const prompt = `Extract key themes and tags from the following text. Respond with ONLY a JSON object in this exact format:
                    {"tags": ["tag1", "tag2", "tag3"], "primaryTheme": "<main theme>"}
                    Limit to 5 most relevant tags. Tags should be lowercase single words or short phrases.
                    Text to analyze: "${text}"
                    Respond with only the JSON object, no other text.`;

    const result = await model.generateContent(prompt);
    const response = await result.response;
    const responseText = response.text().trim();

    const jsonMatch = responseText.match(/\{[\s\S]*\}/);
    if (jsonMatch) {
      const parsed = JSON.parse(jsonMatch[0]);
      return {
        tags: Array.isArray(parsed.tags) ? parsed.tags.slice(0, 5) : [],
        primaryTheme: parsed.primaryTheme || "general",
      };
    }

    throw new Error("Could not parse tags response");
  } catch (error) {
    const err = error as Error;
    logger.error("Tag extraction failed", { error: err.message });
    if (config.fallbackEnabled) {
      return getFallbackTags(text);
    }
    throw error;
  }
};

export const generateInsights = async (
  text: string,
  userId?: string,
): Promise<InsightsResult> => {
  if (!isConfigured || !model) {
    return getFallbackInsights(text);
  }

  try {
    const prompt = `Analyze the following user feedback and provide comprehensive insights. Respond with ONLY a JSON object in this exact format:
                    {
                      "sentimentScore": <number between -1 and 1>,
                      "sentimentLabel": "<positive|negative|neutral|mixed>",
                      "tags": ["tag1", "tag2", "tag3"],
                      "engagementLevel": "<Low|Medium|High|VeryHigh>",
                      "summary": "<brief 1-2 sentence summary>",
                      "actionItems": ["suggestion1", "suggestion2"]
                    }

                    Guidelines:
                    - sentimentScore: -1 is very negative, 0 is neutral, 1 is very positive
                    - engagementLevel: Based on how engaged/invested the user seems
                    - tags: Extract 3-5 key themes, lowercase
                    - actionItems: 1-2 actionable suggestions based on feedback

                    User feedback: "${text}"

                    Respond with only the JSON object, no other text.`;

    const result = await model.generateContent(prompt);
    const response = await result.response;
    const responseText = response.text().trim();

    const jsonMatch = responseText.match(/\{[\s\S]*\}/);
    if (jsonMatch) {
      const parsed = JSON.parse(jsonMatch[0]);

      const insights = {
        sentimentScore: Math.max(-1, Math.min(1, parsed.sentimentScore || 0)),
        sentimentLabel: parsed.sentimentLabel || "neutral",
        tags: Array.isArray(parsed.tags) ? parsed.tags.slice(0, 5) : [],
        engagementLevel: validateEngagementLevel(parsed.engagementLevel),
        summary: parsed.summary || "",
        actionItems: Array.isArray(parsed.actionItems)
          ? parsed.actionItems.slice(0, 3)
          : [],
        analyzedAt: new Date().toISOString(),
      };

      const feedbackId = randomUUID();

      //Async worker
      processEmbeddingJob({
        feedbackId,
        userId,
        text,
        insights,
      }).catch(() => {});

      return {
        sentimentScore: Math.max(-1, Math.min(1, parsed.sentimentScore || 0)),
        sentimentLabel: parsed.sentimentLabel || "neutral",
        tags: Array.isArray(parsed.tags) ? parsed.tags.slice(0, 5) : [],
        engagementLevel: validateEngagementLevel(parsed.engagementLevel),
        summary: parsed.summary || "",
        actionItems: Array.isArray(parsed.actionItems)
          ? parsed.actionItems.slice(0, 3)
          : [],
        analyzedAt: new Date().toISOString(),
      };
    }

    throw new Error("Could not parse insights response");
  } catch (error) {
    const err = error as Error;
    logger.error("Insights generation failed", { error: err.message, userId });
    if (config.fallbackEnabled) {
      return getFallbackInsights(text);
    }
    throw error;
  }
};

const validateEngagementLevel = (
  level: string,
): "Low" | "Medium" | "High" | "VeryHigh" => {
  const validLevels: Array<"Low" | "Medium" | "High" | "VeryHigh"> = [
    "Low",
    "Medium",
    "High",
    "VeryHigh",
  ];
  if (validLevels.includes(level as "Low" | "Medium" | "High" | "VeryHigh")) {
    return level as "Low" | "Medium" | "High" | "VeryHigh";
  }
  return "Medium";
};

const getFallbackSentiment = (text: string): SentimentResult => {
  const positiveWords = [
    "good",
    "great",
    "excellent",
    "love",
    "enjoy",
    "happy",
    "amazing",
    "wonderful",
  ];
  const negativeWords = [
    "bad",
    "poor",
    "terrible",
    "hate",
    "confusing",
    "frustrated",
    "awful",
    "horrible",
  ];

  const lowerText = text.toLowerCase();
  let score = 0;

  positiveWords.forEach((word) => {
    if (lowerText.includes(word)) score += 0.2;
  });
  negativeWords.forEach((word) => {
    if (lowerText.includes(word)) score -= 0.2;
  });

  score = Math.max(-1, Math.min(1, score));

  return {
    score,
    label: score > 0.1 ? "positive" : score < -0.1 ? "negative" : "neutral",
    confidence: 0.5,
    isFallback: true,
  };
};

const getFallbackTags = (text: string): TagExtractionResult => {
  const commonThemes: Record<string, string[]> = {
    product: ["product", "feature", "functionality"],
    usability: ["usability", "easy", "difficult", "confusing", "intuitive"],
    support: ["support", "help", "service", "response"],
    onboarding: ["onboarding", "getting started", "tutorial", "learning"],
    performance: ["fast", "slow", "performance", "speed"],
    pricing: ["price", "cost", "expensive", "cheap", "value"],
  };

  const lowerText = text.toLowerCase();
  const tags: string[] = [];

  Object.entries(commonThemes).forEach(([theme, keywords]) => {
    if (keywords.some((kw) => lowerText.includes(kw))) {
      tags.push(theme);
    }
  });

  return {
    tags: tags.length > 0 ? tags : ["general"],
    primaryTheme: tags[0] || "general",
    isFallback: true,
  };
};

const getFallbackInsights = (text: string): InsightsResult => {
  const sentiment = getFallbackSentiment(text);
  const tagsResult = getFallbackTags(text);

  let engagementLevel: "Low" | "Medium" | "High" | "VeryHigh" = "Medium";
  if (text.length > 200) engagementLevel = "High";
  if (text.length < 50) engagementLevel = "Low";

  return {
    sentimentScore: sentiment.score,
    sentimentLabel: sentiment.label,
    tags: tagsResult.tags,
    engagementLevel,
    summary: "Fallback analysis - AI service unavailable",
    actionItems: ["Review feedback manually"],
    analyzedAt: new Date().toISOString(),
    isFallback: true,
  };
};
