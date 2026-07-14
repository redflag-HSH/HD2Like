# HD2Like

Helldivers 2에서 영감을 받은 멀티플레이 협동 TPS 프로토타입입니다.

## 개요

- **엔진**: Unity 6000.3.11f1
- **네트워킹**: Netcode for GameObjects + Vivox(음성/채팅)
- Addressables 기반 리소스 관리
- 적 AI, 투사체/무기 시스템, 상호작용 오브젝트(종, 모닥불, 울타리 등) 구현

## 주요 구성

- `Assets/00.Scripts/TpsGame` — TPS 게임플레이 (Player, Entity, AI, 무기/투사체, VFX)
- `Assets/00.Scripts/Network` / `Chat` — 멀티플레이 세션 및 채팅
- `Assets/00.Scripts/Manager` / `Item` — 게임 매니저 및 아이템 시스템
- `Assets/06.ItemInfos` — 아이템 데이터
