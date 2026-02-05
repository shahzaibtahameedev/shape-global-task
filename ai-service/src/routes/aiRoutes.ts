import { Router, Response, NextFunction } from 'express';
import { body, validationResult } from 'express-validator';
import * as aiController from '../controllers/aiController';
import { CorrelatedRequest, ApiResponse } from '../types';

const router = Router();

// Validation rules for text input
const validateText = [
  body('text')
    .notEmpty()
    .withMessage('Text is required')
    .isString()
    .withMessage('Text must be a string')
    .isLength({ min: 1, max: 10000 })
    .withMessage('Text must be between 1 and 10000 characters')
];

// Validation error handler
const handleValidation = (
  req: CorrelatedRequest,
  res: Response,
  next: NextFunction
): void => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    const response: ApiResponse<null> = {
      success: false,
      error: {
        message: 'Validation failed',
        code: 'VALIDATION_ERROR'
      },
      correlationId: req.correlationId
    };
    res.status(400).json({ ...response, errors: errors.array() });
    return;
  }
  next();
};

// Routes
router.post(
  '/sentiment',
  validateText,
  handleValidation,
  aiController.analyzeSentiment
);

router.post(
  '/tags',
  validateText,
  handleValidation,
  aiController.extractTags
);

router.post(
  '/insights',
  validateText,
  handleValidation,
  aiController.generateInsights
);

export default router;
