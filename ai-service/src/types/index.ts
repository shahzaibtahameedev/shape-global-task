import { Request } from "express";
export interface CorrelatedRequest extends Request {
  correlationId?: string;
}

export interface SentimentResult {
  score: number;
  label: "positive" | "negative" | "neutral" | "mixed";
  confidence: number;
  isFallback?: boolean;
}

export interface TagExtractionResult {
  tags: string[];
  primaryTheme: string;
  isFallback?: boolean;
}
export interface InsightsResult {
  sentimentScore: number;
  sentimentLabel: string;
  tags: string[];
  engagementLevel: "Low" | "Medium" | "High" | "VeryHigh";
  summary: string;
  actionItems: string[];
  analyzedAt: string;
  isFallback?: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: {
    message: string;
    code: string;
  };
  correlationId?: string;
}

export interface HealthStatus {
  status: "healthy" | "unhealthy";
  timestamp: string;
  service: string;
  version: string;
  geminiConfigured: boolean;
}

export interface AppConfig {
  port: number;
  nodeEnv: string;
  geminiApiKey: string | undefined;
  fallbackEnabled: boolean;
  logLevel: string;
  geminiModel: string;
  geminiEmbeddingsModel: string;
}
