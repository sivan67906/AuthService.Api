using AuthService.Application.Common.Interfaces;
using AuthService.Domain.Interfaces;

namespace AuthService.Application.Features.Page.CreatePage;

public sealed class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, PageDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public CreatePageCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<PageDto> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.Page
        {
            Name = request.Name,
            Url = request.Url,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder
        };

        _commandContext.Pages.Add(entity);
        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = entity.Adapt<Domain.Entities.Page>();
        _queryContext.Pages.Add(queryEntity);
        await _queryContext.SaveChangesAsync(cancellationToken);

        return entity.Adapt<PageDto>();
    }
}
