using FluentValidation;
using velcro.Models.DTOs;

namespace velcro.Validators;

public class CreateListRequestValidator : AbstractValidator<CreateListRequest>
{
    public CreateListRequestValidator()
    {
        RuleFor(x => x.BoardId)
            .NotEmpty().WithMessage("L'identifiant du board est requis.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom de la liste est requis.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");
    }
}

public class UpdateListRequestValidator : AbstractValidator<UpdateListRequest>
{
    public UpdateListRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.")
            .When(x => x.Name != null);
    }
}

public class ReorderListsRequestValidator : AbstractValidator<ReorderListsRequest>
{
    public ReorderListsRequestValidator()
    {
        RuleFor(x => x.Lists)
            .NotEmpty().WithMessage("La liste des positions est requise.");

        RuleForEach(x => x.Lists).ChildRules(item =>
        {
            item.RuleFor(x => x.Id).NotEmpty().WithMessage("L'identifiant de la liste est requis.");
            item.RuleFor(x => x.Position).GreaterThanOrEqualTo(0).WithMessage("La position doit être positive.");
        });
    }
}
