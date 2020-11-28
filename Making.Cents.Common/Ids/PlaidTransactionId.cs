namespace Making.Cents.Common.Ids
{
	[StronglyTypedId(backingType: StronglyTypedIdBackingType.String)]
	public partial struct PlaidTransactionId
	{
		public static explicit operator string(PlaidTransactionId plaidTransactionId) =>
			plaidTransactionId.Value;
		public static implicit operator PlaidTransactionId(string plaidTransactionId) =>
			new PlaidTransactionId(plaidTransactionId);
	}
}
