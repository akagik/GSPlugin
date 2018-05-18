# Google Spreadsheet Plugin

Google Spreadsheet を sheetId と gid から取得し、csv などの形式で保存する.
公開済みのスプレッドシートのみ取得可能であることに注意.

## How to use

1. Project View で右クリックし Create/GSPluginSettings を選択する。
2. 作成された設定ファイルに sheetId や gid など各種変数を埋める。
3. Window/GSPlugin を選択し、ウィンドウを開き、Download ボタンを押す.

## TODO
認証をつけて、非公開のスプレッドシートも取得できるようにする。
(ただし、Google Spreadsheet の API が c# 4 以上じゃないと使えない...)
