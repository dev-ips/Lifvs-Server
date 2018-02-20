using FluentValidation;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Validators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Validators
{
    public class ChangeUserProfileValidator : IValidatorService<UserProfileViewModel>
    {
        private readonly IExceptionManager _exception;
        public ChangeUserProfileValidator(IExceptionManager exception)
        {
            _exception = exception;
        }
        public void Validate(UserProfileViewModel input)
        {
            if (input == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

            var validatorResult = new ChangeUserProfileValidator2();
            var validator = validatorResult.Validate(input);
            if (!validator.IsValid)
            {
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", validator.Errors.Select(x => x.ErrorMessage).ToList());
            }
        }
    }
    public class ChangeUserProfileValidator2 : AbstractValidator<UserProfileViewModel>
    {
        public ChangeUserProfileValidator2()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Emailadress kan inte vara tomt.");
            RuleFor(x => x.StreetAddress).NotEmpty().WithMessage("L'adreça del carrer no ha d'estar buida.");
            RuleFor(x => x.AreaAddress).NotEmpty().WithMessage("L'adreça d'àrea no ha d'estar buida.");
            RuleFor(x => x.PostalAddress).NotEmpty().WithMessage("L'adreça postal no ha d'estar buida.");
            RuleFor(x => x.CountryId).NotEmpty().WithMessage("CountryId no ha d'estar buit.");
        }
    }
}
