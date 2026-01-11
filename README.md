# これは何<br>
**国試勉強お尻叩きbot**<br>
<br>
【処理の流れ】<br>
トリガー1：公式LINEのトークルームで集計ボタンをクリック<br>
OR<br>
トリガー2：22:30になる<br>
↓<br>
Notion API<br>
NotionのDBから今日更新されたレコードを取得<br>
↓<br>
集計<br>
↓<br>
LINE messaging API<br>
公式LINEのトークルームに集計結果を送信<br>
<br>
<br>
【実装時の注意】<br>
・ローカルで実装する場合：<br>
→→local.settings.jsonにAPI keyや各種メッセージを設定すること<br>
・Azure functionsにデプロイする場合<br>
→→環境変数にAPI keyや各種メッセージを設定すること



