using System.Text.RegularExpressions;
using FluentValidation;

namespace Application.Users.Commands
{
    public class RegistrationCommandValidation : AbstractValidator<RegistrationCommand>
    {
        public RegistrationCommandValidation()
        {
            CascadeMode = CascadeMode.Continue;
            
            RuleFor(x => x.Email)
                .EmailAddress()
                .NotEmpty();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(5)
                .Must(x => new Regex(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*#?&^_-]{5,32}$").IsMatch(x))
                .WithMessage("Password must contains letters and digits.");
            RuleFor(x => x.UserName)
                .NotEmpty();
        }
    }
}