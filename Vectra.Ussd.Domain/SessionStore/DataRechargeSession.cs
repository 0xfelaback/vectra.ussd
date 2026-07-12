public record DataRechargeSession<T> : SessionBase where T : class
{
    public AirtimeRecharge.AirtimeNetwork? beneficiaryISP { get; set; } = null;
    public IEnumerable<T>? userDataBundleResult { get; set; } = [];
    public T? userSelectedDataPlan = null;
    public List<ServiceAccount>? userAccounts { get; set; } = [];
}