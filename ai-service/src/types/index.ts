import { Request } from 'express';

// Extended Request with correlation ID
export interface CorrelatedRequest extends Request {
  correlationId?: string;
}

// Sentiment Analysis
export interface SentimentResult {
  score: number;
  label: 'positive' | 'negative' | 'neutral' | 'mixed';
  confidence: number;
  isFallback?: boolean;
}

// Tag Extraction
export interface TagExtractionResult {
  tags: string[];
  primaryTheme: string;
  isFallback?: boolean;
}

// Comprehensive Insights
export interface InsightsResult {
  sentimentScore: number;
  sentimentLabel: string;
  tags: string[];
  engagementLevel: 'Low' | 'Medium' | 'High' | 'VeryHigh';
  summary: string;
  actionItems: string[];
  analyzedAt: string;
  isFallback?: boolean;
}

// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: {
    message: string;
    code: string;
  };
  correlationId?: string;
}

// Health Check
export interface HealthStatus {
  status: 'healthy' | 'unhealthy';
  timestamp: string;
  service: string;
  version: string;
  geminiConfigured: boolean;
}

// Configuration
export interface AppConfig {
  port: number;
  nodeEnv: string;
  geminiApiKey: string | undefined;
  fallbackEnabled: boolean;
  logLevel: string;
}
