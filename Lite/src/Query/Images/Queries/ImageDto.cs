namespace Query.Images.Queries;

public readonly record struct ImageDto(
    long Id,
    long UploaderId,
    long AlbumId,
    string Title,
    string[] Tags,
    DateTime UploadedAt,
    DateTime? RemovedAt
);
