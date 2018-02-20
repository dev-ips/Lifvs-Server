using FluentValidation;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Validators.Interfaces;
using System.Linq;

namespace Lifvs.Common.Validators
{
    public class ChangePasswordValidator : IValidatorService<RecoveryCode>
    {
        private readonly IExceptionManager _exception;
        public ChangePasswordValidator(IExceptionManager exception)
        {
            _exception = exception;
        }
        public void Validate(RecoveryCode input)
        {
            if (input == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");
            var validator = new ChangePasswordValidator2();
            var validatorResult = validator.Validate(input);
            if (!validatorResult.IsValid)
            {
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", validatorResult.Errors.Select(x => x.ErrorMessage).ToList());
            }

        }
    }
    class ChangePasswordValidator2 : AbstractValidator<RecoveryCode>
    {
        public ChangePasswordValidator2()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.RecoverCode).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty();
        }
    }
}
