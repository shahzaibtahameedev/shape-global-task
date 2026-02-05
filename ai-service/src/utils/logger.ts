import winston from 'winston';
import { config } from '../config';

const formatMessage = winston.format.printf(({ timestamp, level, message, correlationId, ...meta }) => {
  const corrId = correlationId ? `[${correlationId}]` : '';
  const metaStr = Object.keys(meta).length > 0 ? ` ${JSON.stringify(meta)}` : '';
  return `${timestamp} ${level} ${corrId} ${message}${metaStr}`;
});

export const logger = winston.createLogger({
  level: config.logLevel,
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.errors({ stack: true }),
    winston.format.json()
  ),
  defaultMeta: { service: 'ai-service' },
  transports: [
    new winston.transports.Console({
      format: winston.format.combine(
        winston.format.colorize(),
        formatMessage
      )
    })
  ]
});
