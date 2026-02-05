import request from 'supertest';
import app from '../src/index';

describe('AI Service API', () => {
  describe('GET /health', () => {
    it('should return health status', async () => {
      const response = await request(app)
        .get('/health')
        .expect(200);

      expect(response.body.success).toBe(true);
      expect(response.body.data.status).toBe('healthy');
      expect(response.body.data.service).toBe('AI Service');
    });
  });

  describe('POST /api/ai/sentiment', () => {
    it('should return 400 when text is missing', async () => {
      const response = await request(app)
        .post('/api/ai/sentiment')
        .send({})
        .expect(400);

      expect(response.body.success).toBe(false);
      expect(response.body.error.code).toBe('VALIDATION_ERROR');
    });

    it('should return 400 when text is empty', async () => {
      const response = await request(app)
        .post('/api/ai/sentiment')
        .send({ text: '' })
        .expect(400);

      expect(response.body.success).toBe(false);
    });

    it('should include correlation ID in response', async () => {
      const correlationId = 'test-correlation-123';
      const response = await request(app)
        .post('/api/ai/sentiment')
        .set('X-Correlation-ID', correlationId)
        .send({ text: 'Hello world' });

      expect(response.body.correlationId).toBe(correlationId);
    });
  });

  describe('POST /api/ai/tags', () => {
    it('should return 400 when text is missing', async () => {
      const response = await request(app)
        .post('/api/ai/tags')
        .send({})
        .expect(400);

      expect(response.body.success).toBe(false);
      expect(response.body.error.code).toBe('VALIDATION_ERROR');
    });

    it('should include correlation ID in response', async () => {
      const correlationId = 'test-correlation-456';
      const response = await request(app)
        .post('/api/ai/tags')
        .set('X-Correlation-ID', correlationId)
        .send({ text: 'Sample text' });

      expect(response.body.correlationId).toBe(correlationId);
    });
  });

  describe('POST /api/ai/insights', () => {
    it('should return 400 when text is missing', async () => {
      const response = await request(app)
        .post('/api/ai/insights')
        .send({})
        .expect(400);

      expect(response.body.success).toBe(false);
      expect(response.body.error.code).toBe('VALIDATION_ERROR');
    });

    it('should include correlation ID in response', async () => {
      const correlationId = 'test-correlation-789';
      const response = await request(app)
        .post('/api/ai/insights')
        .set('X-Correlation-ID', correlationId)
        .send({ text: 'Test user data' });

      expect(response.body.correlationId).toBe(correlationId);
    });
  });

  describe('Unknown endpoints', () => {
    it('should return 404 for unknown routes', async () => {
      const response = await request(app)
        .get('/unknown')
        .expect(404);

      expect(response.body.success).toBe(false);
      expect(response.body.error.code).toBe('NOT_FOUND');
    });
  });
});
