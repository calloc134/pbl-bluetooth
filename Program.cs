using System.Text;
using System.Text.Json;
using InTheHand.Net.Sockets;


static Task EnumerateDevices()
{

    // 無限ループでデバイスを列挙
    while (true)
    {
        
        Console.WriteLine("[*] デバイスを検知中...");
        BluetoothClient bc = new BluetoothClient();

        // 検知されたデバイスのアドレスを記録
        var devices = bc.DiscoverDevices();


        // 検知されたデバイスのアドレスを記録
        var knownDeviceAddresses = devices.Select(d => d.DeviceAddress.ToString()).ToList();
        
        foreach (var device in devices)
        {
            Console.WriteLine($"[+] デバイスが検知されました: {device.DeviceName} ({device.DeviceAddress})");
        }

        // データを送信
        SendDevicesData(knownDeviceAddresses).Wait();
    }
}

static async Task SendDevicesData(List<string> knownDeviceAddresses)
{

    HttpClient httpClient = new HttpClient();
    string graphqlUrl = "https://pbl-gairon-test.calloc134personal.workers.dev/attendances-endpoint";


    var payload = new
    {
        // 検知されたデバイスのアドレスをJSONに含める
        device_ids = knownDeviceAddresses
    };

    // JSONにシリアライズ
    string json = JsonSerializer.Serialize(payload);

    // JSON APIにPOSTリクエストを送信
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await httpClient.PostAsync(graphqlUrl, content);

    // リクエストが成功したかどうかを確認
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("[+] データの送信に成功しました。");
    }
    else
    {
        Console.WriteLine($"[-] データの送信に失敗しました。ステータスコード: {response.StatusCode}");
    }
}

Console.WriteLine("*** 出席管理システム ATTENDIVE デバイス検知システム ***");

// EnumerateDevicesメソッドを一度だけ並行で実行
Task.Run(() => EnumerateDevices());

Console.WriteLine("[*] Enterキーを押すと終了します。");
Console.ReadLine();