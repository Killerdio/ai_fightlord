using FluentValidation;
using FightLord.Application.Commands;

namespace FightLord.Application.Validators
{
    public class PlayCardCommandValidator : AbstractValidator<PlayCardCommand>
    {
        public PlayCardCommandValidator()
        {
            RuleFor(x => x.PlayerId).GreaterThan(0);
            RuleFor(x => x.CardIds).NotEmpty();
        }
    }
}
