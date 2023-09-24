# TradeSim

## Commands

### Start
Start at a specific price level

```.start <Price>```

![image](https://github.com/olibanjoli/TradeSim/assets/1844103/73f3ebdc-34c5-4042-88c7-f5745b80f984)

### Pause
Before next tick, pause so no more orders are accepted from reactions

```.pause```

### Tick
Add value of next candle

```.tick <Price>```

![image](https://github.com/olibanjoli/TradeSim/assets/1844103/b9fce50a-445f-454a-b60c-6d3fec914cc8)

### Scores
Show all scores

```.scores```

![image](https://github.com/olibanjoli/TradeSim/assets/1844103/a660a568-aecd-49a4-b2a5-02730438e337)


### Orders
Show all open orders

```.orders```

### Close
Close all orders with given price. Useful for last candle in simulation.

```.close <Price>```

![image](https://github.com/olibanjoli/TradeSim/assets/1844103/94741daa-c20c-448c-8edc-64b55c92809c)

### Set Score
Set current score of a user to a specific value. Any open orders will stay open.

```.set-score @user 1337```

### Remove Order
Remove an open order of a user

```.remove-order @user```
