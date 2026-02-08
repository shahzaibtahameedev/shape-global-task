import { GoogleGenerativeAI, GenerativeModel } from "@google/generative-ai";
import { config } from "../config";
import { logger } from "../utils/logger";

export let genAI: GoogleGenerativeAI | null = null;
export let model: GenerativeModel | null = null;
export let isConfigured = false;
export let embeddingsModel: GenerativeModel | null = null;

const initialize = (): void => {
  if (config.geminiApiKey) {
    genAI = new GoogleGenerativeAI(config.geminiApiKey);
    model = genAI.getGenerativeModel({ model: config.geminiModel });
    embeddingsModel = genAI.getGenerativeModel({
      model: config.geminiEmbeddingsModel,
    });

    isConfigured = true;

    logger.info("Gemini AI initialized successfully");
  } else {
    logger.warn("Gemini API key not configured - using fallback responses");
    isConfigured = false;
  }
};

initialize();

export const isGeminiConfigured = (): boolean => isConfigured;
