import {
  ensureCollectionExists,
  storeFeedbackEmbedding,
} from "../services/embeddingService";
import { logger } from "../utils/logger";

export type EmbeddingJob = {
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
};

export const processEmbeddingJob = async (job: EmbeddingJob) => {
  try {
    await ensureCollectionExists();
    await storeFeedbackEmbedding(job);
  } catch (error) {
    logger.error("Embedding job failed", {
      feedbackId: job.feedbackId,
      error,
    });

    throw error;
  }
};
