
select
	`ExpiredMoment`, `UserId`
	from AccessToken where `Value`=@Value;
