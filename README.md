Simple authorization service with JWT authentication, Clean Architecture, CQRS.
Базовый флоу: Регистрация -> Логин (получаем RefreshToken в куки) -> запрос на получение AccessToken (GetAccessToken)