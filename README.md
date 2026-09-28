# Nocturne Add New Skills

『真・女神転生III NOCTURNE HD REMASTER』（Steam版）向けのMelonLoader MODです。レベルアップ時のスキル強化（Skill Power-Up）とスキル変化（Skill Mutation）で、**元のスキルを残したまま、変化後のスキルを新しいスキルとして習得**できるようにします。SMT V以降のような、より現代的なスキル管理をSMT III HDへ取り入れることを目的としています。

現在のバージョンは **0.1.0**（初回公開）です。

> **English summary**: A MelonLoader mod for Shin Megami Tensei III: Nocturne HD Remaster (Steam). When a skill is powered up or mutates on level-up, the demon keeps the original skill and learns the new one as an additional skill. When all 8 slots are full, the game's own "forget a skill" screen is used. Also adds Power-Up / Mutation chance control (0% / Native / 100%), an unlimited Power-Up repeat option, and a global ON/OFF toggle. Works standalone; Nocturne Modern Controller v3.0.0 is optional and only adds an in-game settings GUI.

## 主な機能

- **スキルを新規習得**: Skill Power-Up / Skill Mutation が起きたとき、元のスキルを上書きせず、変化後のスキルを新しいスキルとして追加で習得します。
- **8枠満杯時はゲーム本来の忘却画面**: スキルが8つ埋まっている場合は、ゲーム標準の「忘れるスキルを選ぶ」画面で、忘れるスキルを1つ選びます。
- **Skill Power-Up の発生確率**: 0% / 通常 / 100% から選択。
- **Skill Mutation の発生確率**: 0% / 通常 / 100% から選択。
- **Skill Power-Up の繰り返し**: 通常 / 無制限 から選択。
- **MOD全体のON/OFF**: OFFにするとゲーム本来の挙動に戻ります。各設定値はそのまま保持されます。
- **Nocturne Modern Controller v3.0.0 との連携（任意）**: 導入している場合、Controllerの設定GUI「MOD機能」タブから設定できます。

## 必要環境

- Steam版 SMT3 Nocturne HD Remaster（Windows x64）
- MelonLoader 0.6.x（動作確認: v0.6.1 Open-Beta）

Nocturne Modern Controller は**必須ではありません**。このMODは単体で動作します。

## インストール

1. `NocturneAddNewSkills-v0.1.0.zip` をゲームフォルダー（`smt3hd`）へそのまま展開します。ZIPの中は`Mods/`から始まる構成です。

   ```text
   smt3hd/
     Mods/
       NocturneAddNewSkills.dll
   ```

2. ゲームを起動します。初回起動時に設定ファイル`Mods/NocturneAddNewSkills.settings.json`が作られます（ZIPには含まれません）。

## 設定

| 項目 | 設定ファイルのキー | 値 | 既定 |
|---|---|---|---|
| Add New Skills（MOD全体のON/OFF） | `Enabled` | `true` / `false` | `true` |
| Skill Mutation: Chance | `SkillMutation.Chance` | `Disabled`（0%） / `Native`（通常） / `Always`（100%） | `Native` |
| Skill Power-Up: Chance | `SkillPowerUp.Chance` | `Disabled`（0%） / `Native`（通常） / `Always`（100%） | `Native` |
| Skill Power-Up: Repeat | `SkillPowerUp.Repeat` | `Native`（通常） / `Unlimited`（無制限） | `Native` |

設定ファイルの例（既定値）:

```json
{
  "Enabled": true,
  "SkillMutation": {
    "Chance": "Native"
  },
  "SkillPowerUp": {
    "Chance": "Native",
    "Repeat": "Native"
  }
}
```

### 各項目の意味

- **Add New Skills**: MOD全体のON/OFFです。OFFにするとスキルの新規習得・確率変更・繰り返し変更がすべて止まり、ゲーム本来の挙動になります。Chance / Repeat の設定値は変更されずに保持され、ONに戻すとそのまま使われます。
- **Skill Mutation: Chance**
  - 0%: スキル変化を起こしません。
  - 通常: ゲーム本来の確率です。
  - 100%: 変化できるスキル候補がある場合、必ず変化を試みます（成立するかどうかはゲーム本来の判定に従います）。
- **Skill Power-Up: Chance**
  - 0%: スキル強化を起こしません。
  - 通常: ゲーム本来の確率です。
  - 100%: レベルアップ時のスキル変化判定1回につき最大1回、スキルの変化を保証します（1回の戦闘で複数レベル上がった場合も、ゲーム本来と同じく判定は1回です）。強化できるスキルがあればSkill Power-Upを、強化できるスキルが無い場合は変化できるスキルがあればSkill Mutationを起こします。
- **Skill Power-Up: Repeat**
  - 通常: ゲーム本来の挙動です（一度強化したあとは、同じ仲魔で続けて強化が起きにくくなる制限があります）。
  - 無制限: その制限による不成立だけを解除し、続けて強化が起きるようにします。

### 設定の変更方法

- **Nocturne Modern Controller v3.0.0 を導入している場合**: 設定GUIの「MOD機能」タブに「Nocturne Add New Skills」のカードが表示され、ゲーム中に変更できます。変更は即時に反映され、設定ファイルにも保存されます。
- **Controllerを導入していない場合**: `Mods/NocturneAddNewSkills.settings.json` をテキストエディタで編集し、ゲームを再起動してください。不正な値は`Native`（通常）として扱われます。

## Nocturne Modern Controller との連携

このMODはController DLLを参照せず、単体で動作します。Nocturne Modern Controller v3.0.0以降が導入されている場合のみ、`Mods`フォルダー内の機能情報ファイル（`NocturneModernAddNewSkills.features.json`、自動生成）を通じて、設定GUIの「MOD機能」タブから設定できるようになります。

- Controller: https://github.com/tanatyucom/NocturneModernController

## アンインストール / 無効化

- **一時的に無効化**: 設定の「Add New Skills」をOFFにします（設定ファイルでは`"Enabled": false`）。
- **アンインストール**: `Mods`フォルダーから次のファイルを削除します。
  - `NocturneAddNewSkills.dll`
  - `NocturneAddNewSkills.settings.json`（設定を残さない場合）
  - `NocturneModernAddNewSkills.features.json`（Controller連携用の自動生成ファイル）

MODで習得したスキルは、アンインストール後もセーブデータに残ります。

## 注意事項・既知の制限

- 導入前・設定変更前にセーブデータのバックアップを推奨します。
- MODがONの間に「元のスキル」と「強化後のスキル」を両方習得した仲魔は、MODをOFFにした状態でゲーム本来のSkill Power-Upが起きると、同じスキルを重複して持つ可能性があります（現時点で実際の発生は確認されていません）。

## ソースコード / ライセンス

- Source: https://github.com/tanatyucom/NocturneAddNewSkills
- License: [MIT License](LICENSE)

## クレジット

- Author: Gray Ghost（GitHub: tanatyucom）
- [MelonLoader](https://github.com/LavaGang/MelonLoader) / [HarmonyLib](https://github.com/pardeike/Harmony) を利用しています（配布物には含まれません）。

## 開発者向け情報

### ビルド

net6ベースのMelonLoader / Il2CppInteropアセンブリを使用します。MelonLoaderを導入したSMT3HDが生成したIl2Cppアセンブリが必要です。既定の`GameDir`はSteam標準配置（`C:\Program Files (x86)\Steam\steamapps\common\smt3hd`）を参照します。

```powershell
dotnet build -c Release
# 別の場所にゲームがある場合
dotnet build -c Release -p:GameDir="D:\SteamLibrary\steamapps\common\smt3hd"
```

### 調査記録

ネイティブ解析（GameAssembly.dll / global-metadata.dat）と設計検討の記録は`docs/`と`investigations/`にあります。

- [`docs/investigation-log.md`](docs/investigation-log.md)
- [`docs/ROADMAP.md`](docs/ROADMAP.md)

本プロジェクトは旧名`NocturneModernGameplay`として開発されていました。調査記録内の旧名は当時の記録としてそのまま残しています。
