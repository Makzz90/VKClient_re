namespace VKClient.Common.Backend.DataObjects
{
  public enum ResultCode
  {
    DeserializationError = -10000,
    UploadingFailed = -2,
    CommunicationFailed = -1,
    Succeeded = 0,
    UnknownError = 1,
    AppDisabled = 2,
    UnknownMethod = 3,
    IncorrectSignature = 4,
    UserAuthorizationFailed = 5,
    TooManyRequestsPerSecond = 6,
    NotAllowed = 7,
    FloodControlEnabled = 9,
    InternalServerError = 10,
    CaptchaRequired = 14,
    AccessDenied = 15,
    ValidationRequired = 17,
    DeletedOrBanned = 18,
    ConfirmationRequired = 24,
    WrongParameter = 100,
    OutOfLimits = 103,
    InvalidUserIds = 113,
    InvalidAudioFormat = 123,
    CannotAddYourself = 174,
    UserIsBlackListed = 175,
    PricavySettingsRestriction = 176,
    AccessDeniedExtended = 204,
    PostsLimitOrAlreadyScheduled = 214,
    AudioIsExcludedByRightholder = 270,
    AudioFileSizeLimitReached = 302,
    MaximumLimitReached = 302,
    Unauthorized = 401,
    NotEnoughMoney = 504,
    WrongPhoneNumberFormat = 1000,
    UserAlreadyInvited = 1003,
    PhoneAlreadyRegistered = 1004,
    InvalidCode = 1110,
    BadPassword = 1111,
    Processing = 1112,
    ProductNotFound = 1211,
    VideoNotFound = 1212,
    CatalogIsNotAvailable = 1310,
    CatalogCategoriesAreNotAvailable = 1311,
    WallIsDisabled = 10006,
    NewLongPollServerRequested = 100000,
    WrongUsernameOrPassword = 100001,
    CaptchaControlCancelled = 100002,
    ValidationCancelledOrFailed = 100003,
    ConfirmationCancelled = 100004,
    BalanceRefillCancelled = 100005,
  }
}
