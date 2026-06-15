using FluentValidation;
using JobBoard.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Application.Validators
{
    internal class AuthValidators
    {
    }

    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad soyad tələb olunur.")
                .MinimumLength(2).WithMessage("Ad soyad ən azı 2 simvol olmalıdır.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email tələb olunur.")
                .EmailAddress().WithMessage("Düzgün email formatı daxil edin.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifrə tələb olunur.")
                .MinimumLength(8).WithMessage("Şifrə ən azı 8 simvol olmalıdır.")
                .Matches("[A-Z]").WithMessage("Şifrədə ən azı bir böyük hərf olmalıdır.")
                .Matches("[0-9]").WithMessage("Şifrədə ən azı bir rəqəm olmalıdır.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Şifrələr uyğun gəlmir.");

            RuleFor(x => x.Role)
                .Must(r => r == "candidate" || r == "employer")
                .WithMessage("Rol 'candidate' və ya 'employer' olmalıdır.");
        }
    }

    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Şifrədə ən azı bir böyük hərf olmalıdır.")
                .Matches("[0-9]").WithMessage("Şifrədə ən azı bir rəqəm olmalıdır.");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Şifrələr uyğun gəlmir.");
        }
    }

    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Şifrədə ən azı bir böyük hərf olmalıdır.")
                .Matches("[0-9]").WithMessage("Şifrədə ən azı bir rəqəm olmalıdır.");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Şifrələr uyğun gəlmir.");
        }
    }
}
