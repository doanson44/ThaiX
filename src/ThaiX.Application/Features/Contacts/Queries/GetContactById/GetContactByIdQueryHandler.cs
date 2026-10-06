using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Queries.GetContactById;

/// <summary>
/// Handler for GetContactByIdQuery.
/// </summary>
public sealed class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDetailDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityUserService _identityUserService;

    public GetContactByIdQueryHandler(IApplicationDbContext dbContext, IIdentityUserService identityUserService)
    {
        _dbContext = dbContext;
        _identityUserService = identityUserService;
    }

    public async Task<ContactDetailDto> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .AsNoTracking()
            .Include(c => c.Emails)
            .Include(c => c.Phones)
            .Include(c => c.Addresses)
            .Include(c => c.SocialLinks)
            .Include(c => c.Tags)
            .Include(c => c.BankAccounts)
            .Include(c => c.IdentityDocument)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.Id}' not found.");

        Guid? linkedUserId = null;
        string? linkedUserEmail = null;
        var link = await _dbContext.UserContactLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.ContactId == request.Id, cancellationToken);
        if (link is not null)
        {
            linkedUserId = link.UserId;
            linkedUserEmail = await _identityUserService.GetUserEmailAsync(link.UserId, cancellationToken);
        }

        return new ContactDetailDto
        {
            Id = contact.Id,
            FirstName = contact.FullName.FirstName,
            LastName = contact.FullName.LastName,
            Company = contact.Company,
            JobTitle = contact.JobTitle,
            AvatarUrl = contact.AvatarUrl,
            Birthday = contact.Birthday,
            Notes = contact.Notes,
            IsArchived = contact.IsArchived,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt,
            CustomFields = contact.CustomFields,
            Emails = contact.Emails.Select(e => new ContactEmailDto
            {
                Id = e.Id,
                Value = e.Value,
                IsPrimary = e.IsPrimary
            }).ToList(),
            Phones = contact.Phones.Select(p => new ContactPhoneDto
            {
                Id = p.Id,
                Value = p.Value,
                IsPrimary = p.IsPrimary
            }).ToList(),
            Addresses = contact.Addresses.Select(a => new ContactAddressDto
            {
                Id = a.Id,
                Street = a.Street,
                CountryCode = a.CountryCode,
                CityCode = a.CityCode,
                DistrictCode = a.DistrictCode,
                PostalCode = a.PostalCode,
                IsPrimary = a.IsPrimary
            }).ToList(),
            SocialLinks = contact.SocialLinks.Select(s => new ContactSocialLinkDto
            {
                Id = s.Id,
                Platform = s.Platform,
                Url = s.Url
            }).ToList(),
            Tags = contact.Tags.Select(t => new ContactTagDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToList(),
            BankAccounts = contact.BankAccounts.Select(ba => new ContactBankAccountDto
            {
                Id = ba.Id,
                BankCode = ba.BankCode,
                BranchName = ba.BranchName,
                AccountNumberLast4 = ba.AccountNumberLast4,
                AccountName = ba.AccountName,
                CurrencyCode = ba.CurrencyCode,
                IsPrimary = ba.IsPrimary,
                IsVerified = ba.IsVerified
            }).ToList(),
            IdentityDocuments = contact.IdentityDocument is { } d
                ? [new ContactIdentityDocumentDto
                {
                    Id = d.Id,
                    DocumentType = d.DocumentType,
                    DocumentNumberLast4 = d.DocumentNumberLast4,
                    IssuedBy = d.IssuedBy,
                    IssuedPlace = d.IssuedPlace,
                    IssuedDate = d.IssuedDate,
                    ExpiryDate = d.ExpiryDate
                }]
                : [],
            LinkedUserId = linkedUserId,
            LinkedUserEmail = linkedUserEmail
        };
    }
}
