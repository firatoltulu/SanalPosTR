import React, { useState } from 'react';

import Cards from 'react-credit-cards';
import 'react-credit-cards/es/styles-compiled.css';

import {
    Container,
    CardDataContainer,
    CardData,
    Text,
    InputGroup,
    Input,
    InputContainerFlex,
    InputFlex,
    PaymentButton,
    PaymentText,
    CreditCard,
    Select
} from './styles';

import Header from '../../components/Header';
import Axios from 'axios';
import Popup from 'reactjs-popup';

const getParams = function (url) {
    const params = {};
    const parser = document.createElement('a');
    parser.href = url;
    const query = parser.search.substring(1);
    const vars = query.split('&');
    for (var i = 0; i < vars.length; i++) {
        const pair = vars[i].split('=');
        params[pair[0]] = decodeURIComponent(pair[1]);
    }
    return params;
};

function getRndInteger(min, max) {
    return Math.floor(Math.random() * (max - min)) + min;
}

export class ShadowView extends React.Component {
    attachShadow = (host) => {
        host.attachShadow({ mode: "open" });
    }
    render() {
        const { children } = this.props;
        return <div ref={this.attachShadow}>
            {children}
        </div>;
    }
}

const urlParams = getParams(document.location.search);

if (urlParams.Status) {
    if (urlParams.Status == "Success") {
        alert(urlParams.message + " Tahsilat Yapıldı");
    }
}

export default function CreditCardPage() {
    const [cvc, setCvc] = useState('000');
    const [expiry, setExpiry] = useState('1022');
    const [focus, setFocus] = useState('');
    const [name, setName] = useState('FIRAT OLTULU');
    const [number, setNumber] = useState('5400617020092306');
    const orderId = getRndInteger(100000, 100000000);

    const [useSecure, setuseSecure] = useState(false);
    const [amount, setAmount] = useState(200);
    const [bank, setBank] = useState(2);
    const [installment, setInstallment] = useState(0);
    const [open, setOpen] = useState(false);
    const [htmlContent, setHTML] = useState(false);



    const RenderHTML = (props) => {
        return React.createElement("div", { dangerouslySetInnerHTML: { __html: props.rawHTML } });
    }


    const PaymentButtonClick = (e) => {
        const sendObj = {
            CreditCard: {
                CardNumber: number,
                ExpireMonth: expiry.substring(0, 2),
                ExpireYear: expiry.substring(2),
                CVV2: cvc,
                CardHolderName: name
            },
            Order: {
                OrderId: orderId.toString(),
                Total: parseFloat(amount),
                Comission: 0,
                Installment: installment ? parseInt(installment) : installment
            },
            Use3DSecure: useSecure,
            SessionId: "1",
            SelectedBank: parseInt(bank)

        };

        Axios.post("api/SimplePay/Pay", sendObj).then(response => response.data).then(result => {

            if (result.status) {
                if (result.data.isRedirect === false) {
                    alert(result.data.content + " ile tahsilat yapıldı");
                } else {
                    setHTML(result.data.content);
                    setOpen(true);

                    setTimeout(() => { document.getElementById("simplepaytr").submit() }, 0);

                }
            } else {
                alert(result.errorMessage);
            }

        }).catch(err => {
            alert(err);
        });


    }

    

    return (
        <Container>

            <Header />
            <CardDataContainer>
                <Popup open={open} closeOnDocumentClick>
                    <ShadowView>
                        <RenderHTML rawHTML={htmlContent} />
                    </ShadowView>
                </Popup>
                <CardData>
                    <InputGroup>
                        <Text typing={number}>Kart Numarası</Text>
                        <Input
                            typing={number}
                            value={number}
                            type="text"
                            name="number"
                            maxLength={16}
                            onChange={e => setNumber(e.target.value)}
                            onFocus={e => setFocus(e.target.name)}
                        />
                    </InputGroup>

                    <InputGroup>
                        <Text typing={name}>Kart Sahibi</Text>
                        <Input
                            typing={name}
                            value={name}

                            type="text"
                            name="name"
                            onChange={e => setName(e.target.value)}
                            onFocus={e => setFocus(e.target.name)}
                        />
                    </InputGroup>

                    <InputContainerFlex>
                        <InputFlex>
                            <Text typing={expiry}>Son Kullanım Tarihi</Text>
                            <Input
                                typing={expiry}
                                value={expiry}

                                type="text"
                                name="expiry"
                                onChange={e => setExpiry(e.target.value)}
                                onFocus={e => setFocus(e.target.name)}
                            />
                        </InputFlex>

                        <InputFlex>
                            <Text typing={cvc}>CVV2</Text>
                            <Input
                                typing={cvc}
                                value={cvc}

                                type="text"
                                name="cvc"
                                onChange={e => setCvc(e.target.value)}
                                onFocus={e => setFocus(e.target.name)}
                            />
                        </InputFlex>


                    </InputContainerFlex>
                    <InputGroup>
                        <Text>3D Kullanmak İstiyorum</Text>
                        <Input
                            type="checkbox"
                            name="useSecure"
                            onClick={e => setuseSecure(e.target.checked)}
                            onFocus={e => setFocus(e.target.name)}
                        />
                    </InputGroup>



                    <InputGroup>
                        <Text>Banka Seç</Text>
                        <Select name="name" value={bank} onChange={(e) => setBank(e.target.value)} onFocus={e => setFocus(e.target.name)}>
                            <option value="0" >Ziraat</option>
                            <option value="2" >YKB</option>
                        </Select>
                    </InputGroup>

                    <InputGroup>
                        <Text>Tutar</Text>
                        <Input
                            typing={amount}
                            value={amount}
                            type="number"
                            name="number"
                            min="0.00" max="10000.00" step="0.01"
                            maxLength={16}
                            onChange={e => setAmount(e.target.value)}
                            onFocus={e => setFocus(e.target.name)}
                        />
                    </InputGroup>

                    <InputGroup>
                        <Text>Sipariş Numarası</Text>
                        <Input
                            value={orderId}
                            name="orderId"
                            type="text"
                            readOnly
                        />
                    </InputGroup>


                    <InputGroup>
                        <Text>Taksit Seç</Text>
                        <Select name="name" value={installment} onFocus={e => setFocus(e.target.name)} onChange={(e) => setInstallment(e.target.value)} >
                            <option value="0">Peşin</option>
                            <option value="2">2 Taksit</option>
                            <option value="3">3 Taksit</option>
                            <option value="4">4 Taksit</option>
                        </Select>
                    </InputGroup>
                    <PaymentButton onClick={PaymentButtonClick}>
                        <PaymentText>Ödeme Yap</PaymentText>
                    </PaymentButton>
                </CardData>

                <CreditCard>
                    <Cards
                        cvc={cvc}
                        expiry={expiry}
                        focused={focus}
                        name={name}
                        number={number}
                    />
                </CreditCard>

            </CardDataContainer>

        </Container>
    );
};