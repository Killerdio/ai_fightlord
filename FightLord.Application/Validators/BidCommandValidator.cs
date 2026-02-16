using FluentValidation;
using FightLord.Application.Commands;

namespace FightLord.Application.Validators
{
    public class BidCommandValidator : AbstractValidator<BidCommand>
    {
        public BidCommandValidator()
        {
            RuleFor(x => x.PlayerId).GreaterThan(0);
            RuleFor(x => x.Score).InclusiveBetween(0, 3);
        }
    }
}
