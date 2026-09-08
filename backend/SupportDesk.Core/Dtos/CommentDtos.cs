namespace SupportDesk.Core.Dtos;

public record CommentDto(
    int Id,
    int TicketId,
    string AuthorName,
    string Body,
    DateTime CreatedDate
);

public record CreateCommentDto(
    string AuthorName,
    string Body
);