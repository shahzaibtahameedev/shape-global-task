import express, { Response } from 'express';
import cors from 'cors';
import helmet from 'helmet';
import morgan from 'morgan';

import { config } from './config';
import { logger } from './utils/logger';
import { correlationIdMiddleware } from './middleware/correlationId';
import { errorHandler } from './middleware/errorHandler';
import aiRoutes from './routes/aiRoutes';
import * as aiController from './controllers/aiController';
import { CorrelatedRequest, ApiResponse } from './types';

const app = express();

app.use(helmet());
app.use(cors());
app.use(express.json({ limit: '1mb' }));
app.use(express.urlencoded({ extended: true }));

app.use(morgan('combined', {
  stream: { write: (message: string) => logger.info(message.trim()) }
}));

app.use(correlationIdMiddleware);

app.get('/health', aiController.healthCheck);
app.use('/api/ai', aiRoutes);

app.use((req: CorrelatedRequest, res: Response) => {
  const response: ApiResponse<null> = {
    success: false,
    error: { message: 'Endpoint not found', code: 'NOT_FOUND' },
    correlationId: req.correlationId
  };
  res.status(404).json(response);
});

app.use(errorHandler);

const server = app.listen(config.port, () => {
  logger.info(`AI Service started on port ${config.port}`, {
    environment: config.nodeEnv,
    geminiConfigured: !!config.geminiApiKey
  });
});

process.on('SIGTERM', () => {
  logger.info('SIGTERM received, shutting down gracefully');
  server.close(() => {
    logger.info('Server closed');
    process.exit(0);
  });
});

export default app;
