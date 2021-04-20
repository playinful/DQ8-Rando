import os

dat = bytearray(open('encount.tbl','rb').read())

dat = dat[16:]

datnew = []
for i in range(int(len(dat) / 96)):
  datnew.append(dat[i*96:(i+1)*96])

datnew = list(map(lambda x: x[12:-4],datnew))

datfinal = []
for i in datnew:
  for j in range(int(len(i) / 8)):
    datfinal.append(i[j*8:(j+1)*8])

def formatBytes(x):
  h1 = x[2]
  h2 = x[3]
  h1 = hex(h1).split('x')[-1]
  h2 = hex(h2).split('x')[-1]
  h1 = ('0'*(2-len(h1))) + h1
  h2 = ('0'*(2-len(h2))) + h2
  h3 = x[0]
  h4 = x[1]
  h3 = hex(h3).split('x')[-1]
  h4 = hex(h4).split('x')[-1]
  h3 = ('0'*(2-len(h3))) + h3
  h4 = ('0'*(2-len(h4))) + h4
  h5 = x[4]
  h6 = x[5]
  h7 = x[6]
  h8 = x[7]
  h5 = hex(h5).split('x')[-1]
  h6 = hex(h6).split('x')[-1]
  h7 = hex(h7).split('x')[-1]
  h8 = hex(h8).split('x')[-1]
  h5 = ('0'*(2-len(h5))) + h5
  h6 = ('0'*(2-len(h6))) + h6
  h7 = ('0'*(2-len(h7))) + h7
  h8 = ('0'*(2-len(h8))) + h8
  return ((h2 + h1),(h3),(h4),(h5 + h6 + h7 + h8))

datprint = list(map(formatBytes,datfinal))

datdic = {}
for i,j,k,l in datprint:
  if i in datdic:
    tmp = datdic[i]
    if len(tmp[0]) == 1:
      if j > tmp[0][0]:
        tmp[0][0].append(j)
      else if j < tmp[0][0]:
        tmp[0] = [j,tmp[0][0]]
    else:
      if j > tmp[0][1]:
        tmp[0][1] = j
      else if j < tmp[0][0]:
        tmp[0][0] = j
    
    if len(tmp[1]) == 1:
      if k > tmp[1][0]:
        tmp[1][0].append(k)
      else if k < tmp[1][0]:
        tmp[1] = [k,tmp[1][0]]
    else:
      if k > tmp[1][1]:
        tmp[1][1] = k
      else if k < tmp[1][0]:
        tmp[1][0] = k
    
    if not l in tmp[2]:
      tmp[2].append(l)
    
    datdic[i] = tmp
  else:
    datdic[i] = [[j],[k],[l]]

def formatData(x,y):
  ret = x
  

datprintbutreal = list(map(formatData,datprint))

open('output.txt','w').write('\n'.join(datprint))