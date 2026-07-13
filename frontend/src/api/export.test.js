import { afterEach, describe, expect, it, vi } from 'vitest';
import { exportConversation } from './export';

describe('api/export', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('POST vers export-data avec le format et un corps correctement mappé', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      blob: () => Promise.resolve(new Blob(['x'])),
    });
    vi.stubGlobal('fetch', fetchMock);
    // jsdom n'implémente pas ces API : on les neutralise.
    vi.stubGlobal('URL', {
      createObjectURL: vi.fn(() => 'blob:mock'),
      revokeObjectURL: vi.fn(),
    });

    const conversation = {
      id: 42,
      title: 'Team',
      date: '2026-07-13T10:00:00',
      messages: [
        { id: 1, content: 'a', date: '2026-07-13T10:01:00', senderId: 7, status: 'delivered' },
        { id: 2, content: 'b', date: '2026-07-13T10:02:00', senderId: 9, status: 'read' },
      ],
    };

    await exportConversation(conversation, 'csv');

    const [url, options] = fetchMock.mock.calls[0];
    expect(url).toContain('/dataexport/export-data?option=csv');
    expect(options.method).toBe('POST');

    const body = JSON.parse(options.body);
    expect(body.id).toBe(42);
    // senderId -> userId, delivered -> delivred
    expect(body.messages[0]).toMatchObject({ userId: 7, status: 'delivred' });
    expect(body.messages[1]).toMatchObject({ userId: 9, status: 'read' });
  });
});
