namespace velcro.Models.DTOs;

public record CommentDto(Guid Id, Guid CardId, Guid AuthorId, string AuthorUsername, string Content, DateTime CreatedAt, DateTime UpdatedAt);
public record CreateCommentRequest(string Content);
public record UpdateCommentRequest(string Content);
