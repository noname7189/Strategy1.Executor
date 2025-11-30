# Strategy1.Executor

C#으로 구현된 실시간 암호화폐 자동매매 시스템. Binance Futures API를 활용하여 EMA 기반 트레이딩 전략을 실행합니다.

## 개요

Strategy1.Executor는 Strategy1 포트폴리오의 실시간 트레이딩 컴포넌트입니다. 백테스팅 프레임워크(Strategy1.Analyzer)에서 예제로 사용된 간단한 전략을 실제 시장에서 실행하며, 시스템 다운타임 동안 발생한 시그널을 자동으로 복구하는 기능을 포함합니다.

## 아키텍처

```
┌─────────────────────────────────────────────────────────────────┐
│                      StrategyManager                            │
│                      (Singleton Entry Point)                    │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│                        BaseProvider                              │
│  ┌────────────────────┐  ┌──────────────────────────────────────┐│
│  │ IRestClientAdapter │  │ ISocketClientAdapter                 ││
│  │ - GetListenKey     │  │ - SubscribeToUserDataUpdatesAsync    ││
│  │ - GetAccountInfo   │  │ - SubscribeToKlineUpdatesAsync       ││
│  │ - GetKlinesAsync   │  └──────────────────────────────────────┘│
│  │ - PlaceOrderAsync  │                                          │
│  │ - CancelOrderAsync │                                          │
│  └────────────────────┘                                          │
└──────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                       BaseStreamCore<X, C>                      │
│  - Candle 데이터 관리 (DB 연동)                                  │
│  - Strategy Chain 실행                                          │
│  - Real-time Stream 처리                                        │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│                    BaseStrategy<X, C, I, S>                      │
│  ┌──────────────┐  ┌───────────────┐  ┌─────────────────────────┐│
│  │ IInitProcess │  │IOnlineProcess │  │ IOrderProcess           ││
│  │ - PreInit    │  │ - SameCandle  │  │ - ProcessEnter          ││
│  │ - Init       │  │ - DiffCandle  │  │ - ProcessTakeProfit     ││
│  │ - PostInit   │  │ - NewIndicator│  │ - ProcessLosscut        ││
│  | - ...        |  │ - ...         │  │ - ...                   ││
│  └──────────────┘  └───────────────┘  └─────────────────────────┘│
└──────────────────────────────────────────────────────────────────┘
```

## 핵심 컴포넌트

### Core Layer

|           클래스        |                      역할                    |
|-------------------------|----------------------------------------------|
| `StrategyManager`       | 싱글톤 엔트리포인트, Provider/StreamCore 관리 |
| `BaseProvider`          | REST/WebSocket 클라이언트 추상화              |
| `BaseStreamCore<X,C>`   | 캔들 데이터 및 Strategy Chain 관리            |
| `BaseStrategy<X,C,I,S>` | 전략 로직 추상화 (제네릭 기반)                |

### Implementation Layer

|        클래스       |            역할            |
|---------------------|---------------------------|
| `BinanceProvider`   | Binance Futures API 구현체 |
| `ETH5M_StreamCore`  | ETHUSDT 5분봉 스트림 처리  |
| `Strategy1`         | EMA 크로스오버 전략 구현   |

## 전략 로직

Strategy1은 4개의 EMA(Exponential Moving Average)를 활용한 간단한 트렌드 추종 전략입니다.

### 진입 조건 (Long)
```
이전 캔들: EMA1 < EMA2
현재 캔들: EMA1 > EMA2 > EMA3 > EMA4
```

### 청산 조건
- **익절**: TakeProfit 가격 도달 후 EMA1 < EMA2 전환 시 종가 청산
- **손절**: 저가가 LosscutPrice 이하로 하락 시 즉시 청산

### 가격 계산
```csharp
TakeProfit = Close + Coeff1 * Open + Coeff2 * High + Coeff3 * Low
Losscut = Close - Coeff4 * Low
```

## 주요 기능

### 시스템 복구 (Gap Recovery)
시스템 다운타임 동안 놓친 캔들 데이터를 자동으로 복구하고, 해당 기간의 시그널을 정리 및 생성합니다.

```csharp
// InitWithAdditionalCandles()
int startIndex = IndicatorUtil.AddIndicators(this, Candles, Indicators);
List generatedSignals = GenerateSignalsDuringSystemOff(startIndex);
```

### 주문 상태 관리
WebSocket을 통해 데이터 수신 시, IOrderProcess의 ProcessOnOrderUpdate를 통해 실시간 주문 상태를 추적하며, `InmemoryOrder`로 주문 라이프사이클을 관리합니다.

```csharp
// 주문 체결 시 CounterOrder 생성
InmemoryOrder order = new()
{
    OrderId = result.OrderId,
    CounterOrderId = targetOrder.OrderId,  // 원본 주문 참조
    ...
};
```

### Event Chain 패턴
Strategy 로직을 단계별로 분리하여 유연한 확장이 가능합니다.

```
Init Phase:    PreStrategyInit → InitWith(out)AdditionalCandles → PostStrategyInit
Online Phase:  ProcessWithSameCandle | ProcessWithDifferentCandle → TryToMakeNewIndicator → TryToMakeNewSignal
```

## 사용법

```csharp
StrategyManager manager = StrategyManager.Instance;

IProvider provider = manager.AddProvider(new()
{
    Exchange = Exchange.Binance,
    PublicKey = "YOUR_API_KEY",
    SecretKey = "YOUR_SECRET_KEY",
    OnGetAccountInfo = (data) => { /* 계정 정보 처리 */ },
    OnAccountUpdate = (data) => { /* 계정 업데이트 처리 */ },
    OnListenKeyExpired = (data) => { /* ListenKey 만료 처리 */ },
});

provider
    .AddStreamCore()
    .AddStrategy();

manager.Run(keepRunning: true);
```
