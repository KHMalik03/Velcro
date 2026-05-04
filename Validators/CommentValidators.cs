using FluentValidation;
using velcro.Models.DTOs;

namespace velcro.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Le contenu du commentaire est requis.")
            .MaximumLength(2000).WithMessage("Le commentaire ne peut pas dépasser 2000 caractères.");
    }
}

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Le contenu du commentaire est requis.")
            .MaximumLength(2000).WithMessage("Le commentaire ne peut pas dépasser 2000 caractères.");
    }
}
