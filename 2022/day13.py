from day13_input import input

RIGHT_ORDER = 1
WRONG_ORDER = -1
CONTINUE = 0

def compare(a,b):
  if isinstance(a,list) and isinstance(b,list):
    result = RIGHT_ORDER
    for x,y in zip(a,b):
      result = compare(x,y)
      if result != CONTINUE:
        return result
    return compare(len(a),len(b))
  elif isinstance(a,list):
    return compare(a,[b])
  elif isinstance(b,list):
    return compare([a],b)
  else:
    if a < b:
      return RIGHT_ORDER
    elif a > b:
      return WRONG_ORDER
    else:
      return CONTINUE
    
def solve_part_one():
  result = []
  i = 0
  while i < len(input):
    first = input[i]
    second = input[i+1]
    i += 2
    pair_num = i/2
    if compare(first,second) != WRONG_ORDER:
      result.append(pair_num) 

  print(result)
  print(sum(result))

solve_part_one()

def solve_part_two():
  first_divider_packet = [[2]]
  second_divider_packet = [[6]]
  position_of_first_divider = 1
  position_of_second_divider = 2 # after first divider!
  for p in input:
    if compare(p,first_divider_packet) == RIGHT_ORDER:
      position_of_first_divider += 1
    if compare(p,second_divider_packet) == RIGHT_ORDER:
      position_of_second_divider += 1
  print(position_of_first_divider * position_of_second_divider)

solve_part_two()
