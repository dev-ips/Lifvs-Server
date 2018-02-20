using FluentValidation;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Validators.Interfaces;
using System;
using System.Linq;

namespace Lifvs.Common.Validators
{
    public class UserRegistrationValidator : IValidatorService<RegisterModel>
    {
        private readonly IExceptionManager _exception;
        private readonly IUserRepository _userRepository;
        public UserRegistrationValidator(IExceptionManager exception, IUserRepository userRepository)
        {
            _exception = exception;
            _userRepository = userRepository;
        }
        public void Validate(RegisterModel input)
        {
            if (input == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");
            var validate = new UserRegistrationValidator2(_userRepository);
            var validatorResult = validate.Validate(input);
            if (!validatorResult.IsValid)
            {
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", validatorResult.Errors.Select(x => x.ErrorMessage).ToList());
            }
        }
    }
    class UserRegistrationValidator2 : AbstractValidator<RegisterModel>
    {
        private static readonly string[] DeviceTypes = { "ANDROID", "IOS" };

        public UserRegistrationValidator2(IUserRepository userRepository)
        {
            RuleFor(x => x.UserName).Cascade(CascadeMode.StopOnFirstFailure)
                .EmailAddress()
                .NotEmpty().WithMessage("Användarnamn kan inte vara tomt.");

            RuleFor(x => x.UserName).Cascade(CascadeMode.StopOnFirstFailure)

                .Must(userRepository.isEmailExist).WithMessage("Den här emailadressen används redan av en annan användare.").When(x => string.IsNullOrEmpty(x.AuthId));

            RuleFor(x => x.Password).NotEmpty().WithMessage("Lösenord kan inte vara tomt.").Must(x => x.Length >= 6).WithMessage("Lösenordet måste bestå av minst 6 bokstäver och siffror.").When(x => string.IsNullOrEmpty(x.AuthId));

            RuleFor(x => x.AuthId).NotEmpty().When(x => string.IsNullOrEmpty(x.Password));
            RuleFor(x => x.AuthType).NotEmpty().When(x => string.IsNullOrEmpty(x.Password));


            RuleFor(x => x.DeviceId).NotEmpty().WithMessage("Enhetsid kan inte vara tomt.");
            RuleFor(x => x.DeviceType).NotEmpty().WithMessage("Enhetstyp kan inte var tomt.").Must(ValidateDeviceType).WithMessage("Ogiltig enhetstyp.");
        }
        private static bool ValidateDeviceType(string deviceType)
        {
            return string.IsNullOrEmpty(deviceType) || DeviceTypes.Any(item => string.Equals(item, deviceType, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
