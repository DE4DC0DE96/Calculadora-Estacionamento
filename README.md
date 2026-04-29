# Calculadora de Estacionamento

Aplicacao para calcular o valor de estacionamento por tipo de veiculo, usando tarifas configuraveis em arquivo JSON.

O projeto possui:

- `Estacionamento.Core`: regra de calculo compartilhada.
- `Estacionamento.Web`: site responsivo para desktop e mobile.
- `Estacionamento.WinForms`: versao Windows Forms mantida na solution.

## Configuracao de tarifas

As tarifas ficam em `Estacionamento.Web/tarifas-estacionamento.json` para o site e em `Estacionamento.WinForms/tarifas-estacionamento.json` para o WinForms.

```json
{
  "ToleranciaSaidaGratuitaMinutos": 15,
  "ToleranciaDemaisHorasMinutos": 5,
  "Tarifas": {
    "Carro": {
      "PrimeiraHora": 20.00,
      "DemaisHoras": 5.00
    },
    "Moto": {
      "PrimeiraHora": 10.00,
      "DemaisHoras": 3.00
    }
  }
}
```

## Regra de calculo

- Saida precisa ser posterior a entrada.
- Ate 15 minutos de permanencia nao ha cobranca.
- A primeira hora cobrada usa o valor `PrimeiraHora`.
- As horas seguintes usam `DemaisHoras`.
- A tolerancia de 5 minutos e aplicada aos limites das horas cobradas:
  - ate 15 minutos cobra R$ 0,00;
  - acima de 15 minutos ate 1h05 cobra 1 hora;
  - acima de 1h05 ate 2h05 cobra 2 horas;
  - acima de 2h05 ate 3h05 cobra 3 horas.

## Executar site

```powershell
dotnet run --project Estacionamento.Web
```

O campo `Entrada` abre com a data atual as 00:00 e pode ser alterado manualmente.
O campo `Saida` e preenchido com a data/hora atual ao abrir a pagina, mas tambem pode ser alterado manualmente antes do calculo.
Ao clicar em `Calcular valor`, o resultado e atualizado de forma assincrona, sem recarregar a pagina.
Ao trocar entre `Carro` e `Moto`, o valor tambem e recalculado automaticamente.
O botao `Configuracao` abre `/Configuracao` com navegacao suave, onde e possivel alterar tolerancias e tarifas usadas no calculo.
Na versao publicada, as configuracoes ficam armazenadas localmente no arquivo `tarifas-estacionamento.json` ao lado do executavel.

## Executar Windows Forms

```powershell
dotnet run --project Estacionamento.WinForms
```
