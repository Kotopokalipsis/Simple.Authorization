using FluentValidation;

namespace Application.Users.Commands
{
    public class RequestResetPasswordLinkCommandValidation : AbstractValidator<RequestResetPasswordLinkCommand>
    {
        public RequestResetPasswordLinkCommandValidation()
        {
            RuleFor(x => x.Email).EmailAddress().NotEmpty();
        }
    }
}