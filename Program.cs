
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using InTheHand.Net.Sockets;

Console.WriteLine("*** 出席管理システム ATTENDIVE デバイス検知システム ***");

// 授業のUUIDを記述
// ULIDの形式で行われる
Console.WriteLine("[*] 授業のUUIDを入力してください。");
Console.Write(">>> ");
string uuid = Console.ReadLine();

// バリデーション
if (string.IsNullOrEmpty(uuid))
{
    Console.WriteLine("[-] UUIDが空です。");
    return;
}

// ULIDの仕様に準拠しているかどうかを確認
// 正規表現を用いて確認
// 26文字の大文字英数字であることを確認
if (!Regex.IsMatch(uuid, @"^[A-Z0-9]{26}$"))
{
    Console.WriteLine("[-] UUIDが不正です。");
    return;
}

Console.WriteLine("[+] UUIDが正常に入力されました。");
Console.WriteLine("[*] 入力されたUUID: " + uuid);

// EnumerateDevicesメソッドを一度だけ並行で実行
Task.Run(() => EnumerateDevices(uuid));

Console.WriteLine("[*] Enterキーを押すと終了します。");
Console.ReadLine();
Console.WriteLine("[*] 終了します。ご利用ありがとうございました。");

static Task EnumerateDevices(string uuid)
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
        SendDevicesData(knownDeviceAddresses, uuid).Wait();
    }
}

static async Task SendDevicesData(List<string> knownDeviceAddresses, string uuid)
{

    HttpClient http_client = new HttpClient();
    string endpoint_url = "https://pbl-gairon-test.calloc134personal.workers.dev/attendances-endpoint";


    var payload = new
    {
        // 授業のUUIDをJSONに含める
        lesson_uuid = uuid,
        // 検知されたデバイスのアドレスをJSONに含める
        device_ids = knownDeviceAddresses
    };

    // JSONにシリアライズ
    string json = JsonSerializer.Serialize(payload);

    // JSON APIにPOSTリクエストを送信
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var response = await http_client.PostAsync(endpoint_url, content);

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

