
insert
	into Users(`Id`, `Name`, `PasswordHash`, `EMailAddress`, `UserIsActivated`, `UserIsLocked`, `RegistrationMoment`, `TOTPActivated`, `TOTPSecretKey`)
	values (@Id, @Name, @PasswordHash, @EMailAddress, @UserIsActivated, @UserIsLocked, @RegistrationMoment, @TOTPActivated, @TOTPSecretKey);
