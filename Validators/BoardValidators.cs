using FluentValidation;
using velcro.Models.DTOs;

namespace velcro.Validators;

public class CreateBoardRequestValidator : AbstractValidator<CreateBoardRequest>
{
    public CreateBoardRequestValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty().WithMessage("L'identifiant du workspace est requis.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du board est requis.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.BackgroundColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("La couleur doit être au format hexadécimal (#RRGGBB).")
            .When(x => x.BackgroundColor != null);
    }
}

public class UpdateBoardRequestValidator : AbstractValidator<UpdateBoardRequest>
{
    public UpdateBoardRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.")
            .When(x => x.Name != null);

        RuleFor(x => x.BackgroundColor)
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("La couleur doit être au format hexadécimal (#RRGGBB).")
            .When(x => x.BackgroundColor != null);
    }
}
