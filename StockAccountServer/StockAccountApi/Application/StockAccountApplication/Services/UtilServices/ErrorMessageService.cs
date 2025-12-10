namespace StockAccountApplication.Services.UtilServices;

public static class ErrorMessageService
{
    // GENERAL MESSAGES
    public static string InternalServerError500 => "An unexpected error occurred on the server";
    public static string BadRequest400 => "The request could not be understood or was missing required parameters";
    public static string Unauthorized401 => "Authentication is required and has failed or has not yet been provided";
    public static string RestrictedAccess403 => "You do not have permission to access this resource";
    public static string NoValueFoundToUpdate400 => "There is no value found to update";

    // AUTH/TOKEN MESSAGES
    public static string TokenNotFound404 => "Authentication token not found";
    public static string InvalidOrExpiredToken401 => "Invalid or expired authentication token";
    public static string SessionExpired401 => "This token is expired";
    public static string InvalidAuthenticationCredentials401 => "Invalid authentication credentials";

    // USER MESSAGES
    public static string UserNotFound404 => "User not found";
    
    public static string PhoneNumberAlreadyRegistered409 => "Phone number is already registered";
    public static string UsernameAlreadyTaken409 => "Username is already taken";
    
    //EMAIL MESSAGES
    public static string EmailAlreadyRegistered409 => "Email address is already registered";
    public static string EmailNotFound404 => "Email address not found";

    // COMPANY MESSAGES
    public static string CompanyNotFound404 => "Company not found";
    public static string MaxCompanyLimitReached400 => "A user can create a maximum of 3 companies";
    public static string CompanyNameAlredyRegistered409 => "Company name is already registered";
    public static string CountercompanyNotFound404 => "Counter company not found";

    //STOCK MESSAGES
    public static string StockNotFound404 => "Stock not found";
    public static string MaxStockLimitReached400 => "A user can create a maximum of 10 stocks";
    public static string StockNameAlredyRegistered409 => "Stock name is already registered";
    public static string InsufficientStockQuantity400 => "Insufficient stock quantity for the sale transaction";

    //STOCK TRANS MESSAGE
    public static string StockTransNotFound404 => "Stock trans not found";
}
