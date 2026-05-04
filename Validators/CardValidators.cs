using FluentValidation;
using velcro.Models.DTOs;

namespace velcro.Validators;

public class CreateCardRequestValidator : AbstractValidator<CreateCardRequest>
{
    public CreateCardRequestValidator()
    {
        RuleFor(x => x.ListId)
            .NotEmpty().WithMessage("L'identifiant de la liste est requis.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre de la carte est requis.")
            .MaximumLength(200).WithMessage("Le titre ne peut pas dépasser 200 caractères.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("La description ne peut pas dépasser 2000 caractères.")
            .When(x => x.Description != null);

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("La date d'échéance doit être dans le futur.")
            .When(x => x.DueDate.HasValue);
    }
}

public class UpdateCardRequestValidator : AbstractValidator<UpdateCardRequest>
{
    public UpdateCardRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Le titre ne peut pas être vide.")
            .MaximumLength(200).WithMessage("Le titre ne peut pas dépasser 200 caractères.")
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("La description ne peut pas dépasser 2000 caractères.")
            .When(x => x.Description != null);
    }
}

public class MoveCardRequestValidator : AbstractValidator<MoveCardRequest>
{
    public MoveCardRequestValidator()
    {
        RuleFor(x => x.TargetListId)
            .NotEmpty().WithMessage("La liste de destination est requise.");

        RuleFor(x => x.NewPosition)
            .GreaterThanOrEqualTo(0).WithMessage("La position doit être positive ou nulle.");
    }
}
