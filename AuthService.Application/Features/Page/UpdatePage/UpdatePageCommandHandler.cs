using AuthService.Application.Common.Interfaces;
using AuthService.Application.Features.Page.CreatePage;
using AuthService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Page.UpdatePage;

public sealed class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, PageDto>
{
    private readonly ICommandDbContext _commandContext;
    private readonly IQueryDbContext _queryContext;

    public UpdatePageCommandHandler(ICommandDbContext commandContext, IQueryDbContext queryContext)
    {
        _commandContext = commandContext;
        _queryContext = queryContext;
    }

    public async Task<PageDto> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _commandContext.Pages
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            throw new InvalidOperationException($"Page with ID {request.Id} not found");
        }

        entity.Name = request.Name;
        entity.Url = request.Url;
        entity.Description = request.Description;
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _commandContext.SaveChangesAsync(cancellationToken);

        // Sync to query database
        var queryEntity = await _queryContext.Pages
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (queryEntity != null)
        {
            queryEntity.Name = entity.Name;
            queryEntity.Url = entity.Url;
            queryEntity.Description = entity.Description;
            queryEntity.DisplayOrder = entity.DisplayOrder;
            queryEntity.UpdatedAt = entity.UpdatedAt;
            await _queryContext.SaveChangesAsync(cancellationToken);
        }

        return entity.Adapt<PageDto>();
    }
}
