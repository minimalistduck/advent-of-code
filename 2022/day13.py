from input import input

def compare(a,b):
  if isinstance(a,list) and isinstance(b,list):
    if len(a) > len(b):
      return false 
    result = true
    for x,y in zip(a,b):
      result = result or compare(x,y)
    return result
  elif isinstance(a,list):
    y = [b]
    return compare(a,y)
  elif isinstance(b,list):
    x = [a]
    return compare(x,b)
  else
    return a <= b
    

result = 0
i = 0
while i < len(input):
  first = input[i]
  second = input[i+1]
  i += 2
  pair_num = i/2
  if compare(first,second):
    result += pair_num  

print(result)
