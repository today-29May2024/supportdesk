import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TicketService } from './ticket.service';
import { TicketPriority, TicketStatus } from '../models/ticket.model';

describe('TicketService', () => {
  let service: TicketService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TicketService]
    });

    service = TestBed.inject(TicketService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should request tickets with correct search query parameters', () => {
    const mockFilter = { search: 'TCK-2026', page: 1, pageSize: 10 };

    service.getTickets(mockFilter).subscribe();

    const req = httpMock.expectOne((request) => 
      request.url === 'http://localhost:5000/api/tickets' &&
      request.params.get('search') === 'TCK-2026'
    );

    expect(req.request.method).toBe('GET');
  });

  it('should send a PUT request when changing status', () => {
    const ticketId = 1;
    const newStatus = TicketStatus.InProgress;

    service.changeStatus(ticketId, newStatus).subscribe();

    const req = httpMock.expectOne(`http://localhost:5000/api/tickets/${ticketId}/status`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ newStatus: TicketStatus.InProgress });
  });
});