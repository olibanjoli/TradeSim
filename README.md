# TradeSim

Bot to assist in trade simulations.

Sample:
<img width="370" alt="image" src="https://github.com/olibanjoli/TradeSim/assets/1844103/0589beb8-847c-4e44-afb1-ef7c1c1ec5f2">


## Commands

### Start
Start at a specific price level

```/sim start <Price>```

![image](https://github.com/olibanjoli/TradeSim/assets/1844103/73f3ebdc-34c5-4042-88c7-f5745b80f984)

### Pause
Before next tick, pause so no more orders are accepted from reactions

```/sim pause```

### Tick
Add value of next candle

```/sim tick <Price>```

![image](https://github.com/olibanjoli/TradeSim/assets/1844103/b9fce50a-445f-454a-b60c-6d3fec914cc8)

### Scores
Show all scores

```/sim scores```

![image](https://github.com/olibanjoli/TradeSim/assets/1844103/a660a568-aecd-49a4-b2a5-02730438e337)


### Orders
Show all open orders

```/sim orders```

### Close
Close all orders with given price. Useful for last candle in simulation.

```/sim close <Price>```

![image](https://github.com/olibanjoli/TradeSim/assets/1844103/94741daa-c20c-448c-8edc-64b55c92809c)

### Set Score
Set current score of a user to a specific value. Any open orders will stay open.

```/sim set-score @user 1337```

### Remove Order
Remove an open order of a user

```/sim remove-order @user```

### Reset 2x
Each user can 2x once. To allow a user to 2x again:

```/sim reset-2x @user```

### Reset 2x all
Allow everyone to 2x again

```/sim reset-2x-all```

### Reset
Reset the simulation. After reset, start a new session using .start <Price>

```/sim reset```

### Set Tick Value
Set tick value for $ calculation in highscores list

```/sim tick-value <TickValue>```
