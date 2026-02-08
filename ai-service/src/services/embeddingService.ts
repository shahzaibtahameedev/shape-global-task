import { embeddingsModel } from "../infra/geminiClient";
import { qdrantClient } from "../infra/qdrantClient";
import { logger } from "../utils/logger";

const COLLECTION_NAME = "feedback_embeddings";
const VECTOR_SIZE = 768;

export const ensureCollectionExists = async () => {
  const collections = await qdrantClient.getCollections();
  const exists = collections.collections.some(
    (c) => c.name === COLLECTION_NAME,
  );

  if (!exists) {
    await qdrantClient.createCollection(COLLECTION_NAME, {
      vectors: {
        size: VECTOR_SIZE,
        distance: "Cosine",
      },
    });

    logger.info(`Qdrant collection created: ${COLLECTION_NAME}`);
  }
};

const generateEmbedding = async (text: string): Promise<number[]> => {
  const embeddings = embeddingsModel?.embedContent(text);

  return (await embeddings)?.embedding.values || [];
};

export const storeFeedbackEmbedding = async (params: {
  feedbackId: string;
  userId?: string;
  text: string;
  insights: {
    sentimentScore?: number;
    sentimentLabel?: string;
    tags?: string[];
    engagementLevel?: string;
    analyzedAt: string;
  };
}) => {
  const vector = await generateEmbedding(params.text);

  await qdrantClient.upsert(COLLECTION_NAME, {
    points: [
      {
        id: params.feedbackId,
        vector,
        payload: {
          userId: params.userId,
          rawText: params.text,
          sentimentScore: params.insights.sentimentScore,
          sentimentLabel: params.insights.sentimentLabel,
          tags: params.insights.tags,
          engagementLevel: params.insights.engagementLevel,
          analyzedAt: params.insights.analyzedAt,
          model: "stub-v1",
        },
      },
    ],
  });

  logger.info("Feedback embedding stored", {
    feedbackId: params.feedbackId,
  });
};
