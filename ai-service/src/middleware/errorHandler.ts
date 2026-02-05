import { Response, NextFunction } from 'express';
import { logger } from '../utils/logger';
import { CorrelatedRequest, ApiResponse } from '../types';

interface AppError extends Error {
  statusCode?: number;
  code?: string;
}

export const errorHandler = (
  err: AppError,
  req: CorrelatedRequest,
  res: Response,
  _next: NextFunction
): void => {
  logger.error('Unhandled error', {
    correlationId: req.correlationId,
    error: err.message,
    stack: err.stack,
    path: req.path
  });

  const statusCode = err.statusCode || 500;

  const response: ApiResponse<null> = {
    success: false,
    error: {
      message: err.message || 'Internal server error',
      code: err.code || 'INTERNAL_ERROR'
    },
    correlationId: req.correlationId
  };

  res.status(statusCode).json(response);
};
