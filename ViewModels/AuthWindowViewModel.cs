using AvaloniaTestTables.Context;
using AvaloniaTestTables.Models;
using AvaloniaTestTables.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AvaloniaTestTables.ViewModels
{
    public partial class AuthWindowViewModel : ViewModelBase
    {
        private readonly AppDbContext _dbContext;
        private readonly MainWindow _mainWindow;

        [ObservableProperty]
        private string _login;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private bool _isLoginMode = true;

        [ObservableProperty]
        private string _errorMessage;

        public Action CloseCurrentWindow { get; set; }

        public AuthWindowViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;

            // Ensure database is created
            _dbContext.Database.EnsureCreated();
        }

        [RelayCommand]
        private void SwitchMode()
        {
            IsLoginMode = !IsLoginMode;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task Authenticate()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Login and password are required";
                return;
            }

            if (IsLoginMode)
            {
                await LoginUser();
            }
            else
            {
                await RegisterUser();
            }
        }

        [RelayCommand]
        private async Task LoginUser()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль";
                return;
            }

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == Login);

            if (user == null || !VerifyPassword(Password, user.PasswordHash))
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }

            // Закрываем текущее окно авторизации
            CloseCurrentWindow?.Invoke();

            // Создаем и показываем главное окно
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(_dbContext)
            };
            mainWindow.Show();
        }

        private async Task RegisterUser()
        {
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords don't match";
                return;
            }

            if (Password.Length < 6 || !Password.Any(char.IsDigit) || !Password.Any(char.IsLetter))
            {
                ErrorMessage = "Password must be at least 6 characters with at least 1 letter and 1 digit";
                return;
            }

            if (await _dbContext.Users.AnyAsync(u => u.Login == Login))
            {
                ErrorMessage = "Login already exists";
                return;
            }

            var user = new User
            {
                Login = Login,
                PasswordHash = HashPassword(Password)
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            ErrorMessage = "Registration successful. You can now login.";
            IsLoginMode = true;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }
}
