# dfva cliente para C#

Este cliente permite comunicarse con [DFVA](https://github.com/luisza/dfva) para proveer servicios de firma digital para Costa Rica a institutiones.

# Modo de uso 

Este cliente permite:

* Autenticar personas y verificar estado de autenticación
* Firmar documento xml, odf, ms office y verificar estado de firma durante el tiempo que el usuario está firmando
* Validar un certificado emitido con la CA nacional de Costa Rica provista por el BCCR
* Validar un documento XML firmado.
* Revisar si un suscriptor está conectado.

##  Ejemplo de uso

**Nota:** notificationURL debe estar registrado en dfva o ser N/D en clientes no web

```
using dfva_csharp.Properties;
var client = new Client(new Settings());
```
Si se desea autenticar y revisar estado de la autenticación

```
  var response = client.authenticate("0403210121");
  object code;
  response.TryGetValue("id_transaction",out code);

  response = client.authenticate_check(Convert.ToString(code));
  foreach (KeyValuePair<string, object> data in response)
  {
    Console.WriteLine(data.Key+" = "+data.Value);
  }
  Console.WriteLine("Delete: "+client.authenticate_delete(Convert.ToString(code)));
            
```

Si se desea revisar si un suscriptor está conectado

```
Console.WriteLine(client.suscriptor_connected("0808880111"));
```

Si se desea firmar y revisar estado de la firma.

```
  Encoding.UTF8.GetBytes(File.OpenText("test.pdf").ReadToEnd());

  var response = client.sign("080888801111",
        document,
        "pdf",  // puede ser xml_cofirma, xml_contrafirma, odf, msoffice, pdf
        "document test"
         );

  object code;
  response.TryGetValue("id_transaction", out code);
  response = client.sign_check(Convert.ToString(code));
  foreach (KeyValuePair<string, object> data in response)
  {
      Console.WriteLine(data.Key + " = " + data.Value);
  }
  Console.WriteLine("Delete: " + client.sign_delete(Convert.ToString(code)));
```

**Nota:** La revisión de estado de la autenticación/firma no es necesaria en servicios web ya que estos son notificados por en la URL de institución proporcionado.

Si se desea validar un certificado

```
  byte[] document = Encoding.UTF8.GetBytes(File.OpenText("test.crt").ReadToEnd());
  var response = client.validate(document, "certificate");
  foreach (KeyValuePair<string, object> data in response)
  {
      Console.WriteLine(data.Key + " = " + data.Value);
  }
```

Si se desea validar un documento

```
  byte[] document = Encoding.UTF8.GetBytes(File.OpenText("test.pdf").ReadToEnd());
  var response = client.validate(document, "document", "pdf");
  // cofirma, contrafirma, odf, msoffice, pdf
  foreach (KeyValuePair<string, object> data in response)
  {
      Console.WriteLine(data.Key + " = " + data.Value);
  }
```
