import dotenv from 'dotenv';
import { AppConfig } from '../types';

dotenv.config();

export const config: AppConfig = {
  port: parseInt(process.env.PORT || '3001', 10),
  nodeEnv: process.env.NODE_ENV || 'development',
  geminiApiKey: process.env.GEMINI_API_KEY,
  fallbackEnabled: process.env.AI_FALLBACK_ENABLED === 'true',
  logLevel: process.env.LOG_LEVEL || 'info'
};
