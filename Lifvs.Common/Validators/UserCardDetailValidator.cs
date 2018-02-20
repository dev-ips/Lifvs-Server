using FluentValidation;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Validators.Interfaces;
using System.Linq;

namespace Lifvs.Common.Validators
{
    public class UserCardDetailValidator : IValidatorService<UserCardDetailModel>
    {
        private readonly IExceptionManager _exception;
        public UserCardDetailValidator(IExceptionManager exception)
        {
            _exception = exception;
        }
        public void Validate(UserCardDetailModel input)
        {
            if (input == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

            var validator = new UserCardDetailValidator2();
            var validatorResult = validator.Validate(input);
            if (!validatorResult.IsValid)
            {
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", validatorResult.Errors.Select(x => x.ErrorMessage).ToList());
            }
        }
    }
    class UserCardDetailValidator2 : AbstractValidator<UserCardDetailModel>
    {
        public UserCardDetailValidator2()
        {
            RuleFor(x => x.CardNumber).NotEmpty().WithMessage("El número de la targeta no ha d'estar buit.");
            RuleFor(x => x.ExpiredMonth).NotEmpty().WithMessage("El mes caducat no hauria d'estar buit.").Must(x => x > 0);
            RuleFor(x => x.ExpiredYear).NotEmpty().WithMessage("L'any vençut no hauria d'estar buit.").Must(x => x > 0);
            RuleFor(x => x.CVC).NotEmpty().WithMessage("CVC no ha d'estar buit.").Must(x => x > 0);
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("El número de telèfon no ha d'estar buit.");
        }
    }
}
