import { Response, NextFunction } from 'express';
import * as geminiService from '../services/geminiService';
import { logger } from '../utils/logger';
import { CorrelatedRequest, ApiResponse, SentimentResult, TagExtractionResult, InsightsResult, HealthStatus } from '../types';


export const analyzeSentiment = async (
  req: CorrelatedRequest,
  res: Response,
  next: NextFunction
): Promise<void> => {
  try {
    const { text } = req.body as { text: string };

    logger.info('Sentiment analysis requested', {
      correlationId: req.correlationId,
      textLength: text?.length
    });

    const result = await geminiService.analyzeSentiment(text);

    logger.info('Sentiment analysis completed', {
      correlationId: req.correlationId,
      score: result.score,
      label: result.label
    });

    const response: ApiResponse<SentimentResult> = {
      success: true,
      data: result,
      correlationId: req.correlationId
    };

    res.json(response);
  } catch (error) {
    next(error);
  }
};

/**
 * POST /api/ai/tags
 * Extract tags/themes from text
 */
export const extractTags = async (
  req: CorrelatedRequest,
  res: Response,
  next: NextFunction
): Promise<void> => {
  try {
    const { text } = req.body as { text: string };

    logger.info('Tag extraction requested', {
      correlationId: req.correlationId,
      textLength: text?.length
    });

    const result = await geminiService.extractTags(text);

    logger.info('Tag extraction completed', {
      correlationId: req.correlationId,
      tagsCount: result.tags?.length
    });

    const response: ApiResponse<TagExtractionResult> = {
      success: true,
      data: result,
      correlationId: req.correlationId
    };

    res.json(response);
  } catch (error) {
    next(error);
  }
};

/**
 * POST /api/ai/insights
 * Generate comprehensive user insights
 */
export const generateInsights = async (
  req: CorrelatedRequest,
  res: Response,
  next: NextFunction
): Promise<void> => {
  try {
    const { text, userId } = req.body as { text: string; userId?: string };

    logger.info('Insights generation requested', {
      correlationId: req.correlationId,
      userId,
      textLength: text?.length
    });

    const result = await geminiService.generateInsights(text, userId);

    logger.info('Insights generation completed', {
      correlationId: req.correlationId,
      userId,
      sentimentScore: result.sentimentScore,
      engagementLevel: result.engagementLevel
    });

    const response: ApiResponse<InsightsResult> = {
      success: true,
      data: result,
      correlationId: req.correlationId
    };

    res.json(response);
  } catch (error) {
    next(error);
  }
};


export const healthCheck = (
  _req: CorrelatedRequest,
  res: Response
): void => {
  const health: HealthStatus = {
    status: 'healthy',
    timestamp: new Date().toISOString(),
    service: 'ai-service',
    version: '1.0.0',
    geminiConfigured: geminiService.isGeminiConfigured()
  };

  res.json(health);
};
